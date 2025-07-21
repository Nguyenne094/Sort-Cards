using UnityEngine;
using UnityEngine.UI;

namespace Bap.EventChannel
{
    [CreateAssetMenu(fileName = "GeneralMethods", menuName = "GeneralMethods", order = 0)]

    public class GeneralMethodsSO : DescriptionAbstractSO
    {
        public enum FPS
        {
            Hight = 120,
            Medium = 60,
            Low = 30
        }

        public void SetFPS(int value)
        {
            FPS fps = FPS.Hight;
            switch (value)
            {
                case 0:
                    fps = FPS.Hight;
                    break;
                case 1:
                    fps = FPS.Medium;
                    break;
                case 2:
                    fps = FPS.Low;
                    break;
            }

            Application.targetFrameRate = (int)fps;
        }
    }
}