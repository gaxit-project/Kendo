using UnityEngine;

namespace InGame.Model
{
    public class PhysicsModel
    {
        private Vector3 _currentVelocity;
        private bool _isKnockback;
        private int _bounceCount;

        public PhysicsModel()
        {
            _currentVelocity = Vector3.zero;
            _isKnockback = false;
            _bounceCount = 0;
        }

        // アクセサメソッド
        public Vector3 GetCurrentVelocity() => _currentVelocity;
        public void SetCurrentVelocity(Vector3 velocity) => _currentVelocity = velocity;
        public bool GetIsKnockback() => _isKnockback;
        public void SetIsKnockback(bool value) => _isKnockback = value;
        public int GetBounceCount() => _bounceCount;
        public void SetBounceCount(int count) => _bounceCount = count;
        public void IncrementBounceCount() => _bounceCount++;
        
        // 物理計算
        public void UpdateVelocityWithDrag(float drag)
        {
            _currentVelocity -= _currentVelocity * drag * Time.deltaTime;
        }
        
        public void CalculateReflectionVelocity(Vector3 v1, Vector3 v2, float m1, float m2, float e, Vector3 normal, out Vector3 newV1, out Vector3 newV2)
        {
            float v1n_scalar = Vector3.Dot(v1, normal);
            float v2n_scalar = Vector3.Dot(v2, normal);
            Vector3 v1t_vec = v1 - v1n_scalar * normal;
            Vector3 v2t_vec = v2 - v2n_scalar * normal;
            float newV1n_scalar = (m1 * v1n_scalar + m2 * v2n_scalar - m2 * e * (v1n_scalar - v2n_scalar)) / (m1 + m2);
            float newV2n_scalar = (m1 * v1n_scalar + m2 * v2n_scalar + m1 * e * (v1n_scalar - v2n_scalar)) / (m1 + m2);
            newV1 = newV1n_scalar * normal + v1t_vec;
            newV2 = newV2n_scalar * normal + v2t_vec;
        }
    }
}