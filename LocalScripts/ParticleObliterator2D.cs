using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace YippeeKey.LocalScripts
{
    internal class ParticleObliterator2D : MonoBehaviour
    {
        private float deathTime = 2.5f;

        // Update is called once per frame
        void Update()
        {
            deathTime -= Time.deltaTime;

            if (deathTime <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
