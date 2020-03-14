using System;
using System.Collections.Generic;
using UI.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Application
{
    public class QuickSlotManager : MonoBehaviour
    {
        public QuickSlot[] slots = new QuickSlot[ Enum.GetValues( typeof( QuickSlotType ) ).Length ];
        public static QuickSlotManager singleton;

        #region Unity Methods
        private void Awake ( )
        {
            singleton = this;
        }
        #endregion

        #region Private Methods

        #endregion

        #region Public Methods
        public void Init ( )
        {
            ClearSlots();
        }

        public void ClearSlots ( )
        {
            for ( int i = 0; i < slots.Length; i++ )
            {
                slots[ i ].icon.gameObject.SetActive( false );
            }
        }

        public void UpdateSlot ( QuickSlotType type, Sprite icon )
        {
            QuickSlot slot = GetSlot( type );
            slot.icon.sprite = icon;
            slot.icon.gameObject.SetActive( true );
        }

        public QuickSlot GetSlot ( QuickSlotType type )
        {
            return slots[ (int) type ];
        }
        #endregion
    }
}

