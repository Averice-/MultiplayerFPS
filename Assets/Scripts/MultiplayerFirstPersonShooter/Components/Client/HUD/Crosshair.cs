using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ShardStudios {

    public class Crosshair : MonoBehaviour
    {

        [Header("Crosshair Images")]
        public Image crosshairInner;
        public Image crosshairOuter;
        public Color crosshairColor;

        [Space(10)]
        [Header("Crosshair Settings")]
        public float spreadSpeed = 100f;
        public float settleSpeed = 1f;
        public float crosshairMaxSpread = 2.5f;
        public bool scaleInnerWithSpread = false;

        float crosshairSpread = 1f;

        RectTransform crosshairOuterRect;
        RectTransform crosshairInnerRect;
        Vector3 outerOriginalScale;
        Vector3 innerOriginalScale;

        void Awake(){
            crosshairOuterRect = crosshairOuter.GetComponent<RectTransform>();
            crosshairInnerRect = crosshairInner.GetComponent<RectTransform>();

            outerOriginalScale = crosshairOuterRect.localScale;
            innerOriginalScale = crosshairInnerRect.localScale;
        }

        public void SpreadCrosshair(){
            crosshairSpread = Mathf.Clamp(crosshairSpread + (spreadSpeed * Time.deltaTime), 1f, crosshairMaxSpread);
        }

        public void ResetCrosshair(){
            crosshairSpread = 1f;
        }

        void SettleCrosshair(){
            crosshairSpread = Mathf.Clamp(crosshairSpread - (settleSpeed * Time.deltaTime), 1f, crosshairMaxSpread);
        }

        void Update(){

            crosshairOuterRect.localScale = outerOriginalScale * crosshairSpread;
            if( scaleInnerWithSpread )
                crosshairInnerRect.localScale = innerOriginalScale * crosshairSpread;

            SettleCrosshair();

        }


    }

}
