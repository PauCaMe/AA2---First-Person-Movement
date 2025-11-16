using UnityEngine;

public class ObjectPickup : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public LayerMask pickupMask;

    [Header("Pickup")]
    public float pickupDistance = 2f;      // Distancia máxima para coger

    [Header("Hold distance")]
    public float minDistance = 1f;         // Distancia mínima al mantenerlo
    public float maxDistance = 4f;         // Distancia máxima al mantenerlo

    [Header("Hold behaviour")]
    public float followStrength = 10f;     // Qué tan rápido sigue el punto delante de la cámara
    public float throwForce = 10f;         // Fuerza al lanzar el objeto

    Rigidbody heldObject;
    float currentDistance;

    void Update()
    {
        if (heldObject == null)
        {
            // Si no tengo objeto cogido, intento coger uno
            TryPickup();
        }
        else
        {
            // Si tengo un objeto cogido, lo mantengo delante de la cámara
            AdjustDistanceWithScroll();
            HoldObjectWithForces();

            // Soltar normal (sin lanzar) con botón derecho
            if (Input.GetMouseButtonUp(1))
            {
                DropObject();
            }

            // Lanzar con fuerza en la dirección de la cámara al soltar el botón izquierdo
            if (Input.GetMouseButtonUp(0))
            {
                DropObject(true);
            }
        }
    }

    void TryPickup()
    {
        // Solo intenta coger mientras mantienes pulsado el botón izquierdo
        if (!Input.GetMouseButton(0)) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * pickupDistance, Color.yellow);

        if (Physics.Raycast(ray, out hit, pickupDistance, pickupMask))
        {
            Rigidbody rb = hit.rigidbody;

            if (rb != null)
            {
                heldObject = rb;
                currentDistance = hit.distance;

                heldObject.useGravity = false;
                heldObject.linearVelocity = Vector3.zero;
                heldObject.angularVelocity = Vector3.zero;
            }
        }
    }

    void AdjustDistanceWithScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            // Rueda hacia delante = más cerca
            currentDistance -= scroll;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }
    }

    void HoldObjectWithForces()
    {
        if (heldObject == null) return;

        // Punto objetivo delante de la cámara
        Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * currentDistance;
        Vector3 direction = targetPos - heldObject.position;

        // Lo movemos usando velocidad para respetar colisiones
        heldObject.linearVelocity = direction * followStrength;
    }

    void DropObject(bool throwIt = false)
    {
        if (heldObject == null) return;

        heldObject.useGravity = true;

        if (throwIt)
        {
            // Lanzar en la dirección en la que mira la cámara
            heldObject.linearVelocity = playerCamera.transform.forward * throwForce;
        }

        heldObject = null;
    }
}
