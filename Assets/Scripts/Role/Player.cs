using UnityEngine;

namespace MoonFramework.Test
{
    public class Player : MonoBehaviour
    {
        public void Update()
        {
            var hor = Input.GetAxis("Horizontal");
            var ver = Input.GetAxis("Vertical");
            transform.Translate(10f * Time.deltaTime * (Vector3.right * hor + ver * Vector3.forward), Space.Self);
        }
    }
}