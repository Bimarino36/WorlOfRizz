using System;
using UnityEngine;

namespace IdleRestaurant.Gameplay
{
    [Serializable]
    public sealed class OrderTicket
    {
        [SerializeField] private bool requiresKitchen;
        [SerializeField] private bool requiresBar;
        [SerializeField] private int totalPrice;
        [SerializeField] private int baseTipAmount;
        [SerializeField] private int tipAmount;
        [SerializeField] private bool isSubmitted;
        [SerializeField] private float kitchenReadyAt;
        [SerializeField] private float barReadyAt;
        [SerializeField] private bool kitchenPickedUp;
        [SerializeField] private bool barPickedUp;
        [SerializeField] private bool delivered;

        public OrderTicket(bool requiresKitchen, bool requiresBar, int totalPrice, int tipAmount)
        {
            this.requiresKitchen = requiresKitchen;
            this.requiresBar = requiresBar;
            this.totalPrice = totalPrice;
            baseTipAmount = Mathf.Max(0, tipAmount);
            this.tipAmount = tipAmount;
        }

        public bool RequiresKitchen => requiresKitchen;

        public bool RequiresBar => requiresBar;

        public int TotalPrice => totalPrice;

        public int BaseTipAmount => baseTipAmount;

        public int TipAmount => tipAmount;

        public bool IsSubmitted => isSubmitted;

        public bool KitchenPickedUp => kitchenPickedUp;

        public bool BarPickedUp => barPickedUp;

        public bool Delivered => delivered;

        public bool IsReadyToServe
        {
            get
            {
                return (!requiresKitchen || kitchenPickedUp) &&
                       (!requiresBar || barPickedUp);
            }
        }

        public void Submit(float currentTime, float kitchenDelay, float barDelay)
        {
            isSubmitted = true;
            kitchenReadyAt = currentTime + Mathf.Max(0f, kitchenDelay);
            barReadyAt = currentTime + Mathf.Max(0f, barDelay);
        }

        public bool CanPickupKitchen(float currentTime)
        {
            return requiresKitchen && isSubmitted && !kitchenPickedUp && currentTime >= kitchenReadyAt;
        }

        public bool CanPickupBar(float currentTime)
        {
            return requiresBar && isSubmitted && !barPickedUp && currentTime >= barReadyAt;
        }

        public void MarkKitchenPickedUp()
        {
            kitchenPickedUp = true;
        }

        public void MarkBarPickedUp()
        {
            barPickedUp = true;
        }

        public void MarkDelivered()
        {
            delivered = true;
        }

        public void ApplyServiceQuality(float serviceQuality)
        {
            ApplyTipMultiplier(serviceQuality);
        }

        public void ApplyTipMultiplier(float tipMultiplier)
        {
            float clampedMultiplier = Mathf.Clamp01(tipMultiplier);
            tipAmount = Mathf.Max(0, Mathf.RoundToInt(baseTipAmount * clampedMultiplier));
        }
    }
}
