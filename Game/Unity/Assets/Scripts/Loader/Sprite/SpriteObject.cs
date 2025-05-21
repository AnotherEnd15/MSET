using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ET
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteObject : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        private Transform _cameraMainTrans;

        void Awake()
        {
            _spriteRenderer = this.GetComponent<SpriteRenderer>();
            _spriteRenderer.shadowCastingMode = ShadowCastingMode.On;

            _cameraMainTrans = Camera.main.transform;
        }
    }
}
