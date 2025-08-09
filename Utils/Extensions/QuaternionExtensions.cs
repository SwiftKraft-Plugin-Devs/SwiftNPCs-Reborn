using UnityEngine;

namespace SwiftNPCs.Utils.Extensions
{
    public static class QuaternionExtensions
    {
        public static Quaternion SmoothDamp(this Quaternion current, Quaternion target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed = Mathf.Infinity) => current.SmoothDamp(target, ref currentVelocity, smoothTime, Time.fixedDeltaTime, maxSpeed);
        public static Quaternion SmoothDamp(this Quaternion current, Quaternion target, ref Vector3 currentVelocity, float smoothTime, float deltaTime, float maxSpeed = Mathf.Infinity)
        {
            if (Time.deltaTime == 0f)
                return current;

            Vector3 c = current.eulerAngles;
            Vector3 t = target.eulerAngles;

            return Quaternion.Euler(
              Mathf.SmoothDampAngle(c.x, t.x, ref currentVelocity.x, smoothTime, maxSpeed, deltaTime),
              Mathf.SmoothDampAngle(c.y, t.y, ref currentVelocity.y, smoothTime, maxSpeed, deltaTime),
              Mathf.SmoothDampAngle(c.z, t.z, ref currentVelocity.z, smoothTime, maxSpeed, deltaTime)
            );
        }

        public static Quaternion SmoothDampNoZ(this Quaternion current, Quaternion target, ref Vector2 currentVelocity, float smoothTime, float deltaTime, float maxSpeed = Mathf.Infinity)
        {
            if (deltaTime == 0f)
                return current;

            Vector3 c = current.eulerAngles;
            Vector3 t = target.eulerAngles;

            // Smooth only X and Y
            float x = Mathf.SmoothDampAngle(c.x, t.x, ref currentVelocity.x, smoothTime, maxSpeed, deltaTime);
            float y = Mathf.SmoothDampAngle(c.y, t.y, ref currentVelocity.y, smoothTime, maxSpeed, deltaTime);

            return Quaternion.Euler(x, y, 0f);
        }

        public static float NormalizeAngle(this float a) => a > 180f ? a - 360f : a;

        public static bool IsNaN(this Quaternion q) => float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w);
    }
}
