using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animations
{
    [System.Serializable]
    public class EditAnimationSimple
        : EditAnimation
    {
		public AnimationSimpleLEDType ledType;
        public Color32 color;

        public override AnimationType type { get { return AnimationType.Simple; } }
        public override Animation ToAnimation(EditDataSet editSet, DataSet set)
        {
            return new AnimationSimple()
            {
                ledType = this.ledType,
                color = ColorUtils.toColor(this.color.r, this.color.g, this.color.b)
            };
        }

        public override string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public override void FromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
 
        public override EditAnimation Duplicate()
        {
            EditAnimationSimple ret = new EditAnimationSimple();
            ret.name = this.name;
		    ret.duration = this.duration;
            ret.ledType = this.ledType;
            ret.color = this.color;
            return ret;
        }
   }
}