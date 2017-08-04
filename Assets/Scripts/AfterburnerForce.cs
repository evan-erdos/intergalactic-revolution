using System;
using UnityEngine;

[RequireComponent(typeof (SphereCollider))]
public class AfterburnerForce : MonoBehaviour {
    new SphereCollider collider;
    Collider[] colliders;
    [SerializeField] float effectAngle = 15;
    [SerializeField] float effectWidth = 1;
    [SerializeField] float effectDistance = 10;
    [SerializeField] float force = 10;

    void OnEnable() => collider = GetComponent<SphereCollider>();

    void FixedUpdate() {
        colliders = Physics.OverlapSphere(transform.position + collider.center, collider.radius);
        for (var n=0; n<colliders.Length; ++n) {
            if (colliders[n].attachedRigidbody != null) {
                var localPos = transform.InverseTransformPoint(colliders[n].transform.position);
                localPos = Vector3.MoveTowards(localPos, new Vector3(0, 0, localPos.z), effectWidth*0.5f);
                var angle = Mathf.Abs(Mathf.Atan2(localPos.x, localPos.z)*Mathf.Rad2Deg);
                var falloff = Mathf.InverseLerp(effectDistance, 0, localPos.magnitude);
                falloff *= Mathf.InverseLerp(effectAngle, 0, angle);
                var delta = colliders[n].transform.position - transform.position;
                colliders[n].attachedRigidbody.AddForceAtPosition(
                    delta.normalized*force*falloff,
                    Vector3.Lerp(colliders[n].transform.position,transform.TransformPoint(0, 0, localPos.z),0.1f));
            }
        }
    }

    void OnDrawGizmosSelected() {
        if (collider == null) collider = (GetComponent<Collider>() as SphereCollider);
        collider.radius = effectDistance*0.5f;
        collider.center = new Vector3(0, 0, effectDistance*.5f);
        var directions = new Vector3[] {Vector3.up, -Vector3.up, Vector3.right, -Vector3.right};
        var perpDirections = new Vector3[] {-Vector3.right, Vector3.right, Vector3.up, -Vector3.up};
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        for (var n=0; n<4; ++n) {
            var origin = transform.position + transform.rotation*directions[n]*effectWidth*0.5f;
            var direction = transform.TransformDirection(Quaternion.AngleAxis(effectAngle, perpDirections[n])*Vector3.forward);
            Gizmos.DrawLine(origin, origin + direction*collider.radius*2);
        }
    }
}
