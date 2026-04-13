using UnityEngine;

namespace IdleRestaurant.Gameplay
{
    public static class ServiceQualityModel
    {
        public static float EvaluateWaitPhaseQualityFactor(float waitElapsed, float waitLimit, float decayStartNormalized)
        {
            if (waitLimit <= 0.01f)
            {
                return 1f;
            }

            float progress = Mathf.Clamp01(waitElapsed / waitLimit);
            float start = Mathf.Clamp01(decayStartNormalized);
            if (progress <= start)
            {
                return 1f;
            }

            float denominator = Mathf.Max(0.0001f, 1f - start);
            float penaltyProgress = Mathf.Clamp01((progress - start) / denominator);
            return 1f - penaltyProgress;
        }

        public static float EvaluateTipMultiplier(float serviceQuality)
        {
            return Mathf.Clamp01(serviceQuality);
        }

        public static int EvaluateLoyaltyDelta(float serviceQuality, bool walkedOut)
        {
            if (walkedOut)
            {
                return -4;
            }

            float clamped = Mathf.Clamp01(serviceQuality);
            if (clamped >= 0.9f)
            {
                return 3;
            }

            if (clamped >= 0.7f)
            {
                return 2;
            }

            if (clamped >= 0.5f)
            {
                return 1;
            }

            if (clamped >= 0.3f)
            {
                return 0;
            }

            return -2;
        }

        public static string BuildSettlementLog(
            string seatLabel,
            float serviceQuality,
            float tipMultiplier,
            int baseTip,
            int finalTip,
            int loyaltyDelta)
        {
            return
                "Service quality at " +
                seatLabel +
                ": quality=" +
                serviceQuality.ToString("0.00") +
                ", tip x" +
                tipMultiplier.ToString("0.00") +
                " (" +
                baseTip +
                " -> " +
                finalTip +
                "), loyalty " +
                (loyaltyDelta >= 0 ? "+" : string.Empty) +
                loyaltyDelta +
                ".";
        }
    }
}
