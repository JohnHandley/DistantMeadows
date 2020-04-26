using UnityEngine;

namespace DistantMeadows.Actors.Player.Models {
    [System.Serializable]
    public class Controls {
        public Vector2 Move; // Left Stick Axis

        public Vector2 Look; // Right Stick Axis

        [SerializeField]
        private bool _rightHandPrimaryAction; // Right Shoulder
        [SerializeField]
        private bool _rightHandSecondaryAction; // Right Trigger

        [SerializeField]
        private bool _leftHandPrimaryAction; // Left Shoulder
        [SerializeField]
        private bool _leftHandSecondaryAction; // Left Trigger

        [SerializeField]
        private bool _targetLockOrResetCamera; // Right Stick Press

        [SerializeField]
        private bool _interactOrPickup; // South Button
        [SerializeField]
        private bool _useItem; // West Button
        [SerializeField]
        private bool _dodge; // East Button
        [SerializeField]
        private bool _twoHandRightHand; // North Button

        [SerializeField]
        private bool _sprint; // Hold East Button
        [SerializeField]
        private bool _twoHandLeftHand; // Hold North Button

        [SerializeField]
        private bool _block; // Hold Left Shoulder

        public bool RightHandPrimaryAction {
            get => UsedInput( ref _rightHandPrimaryAction );
            set => _rightHandPrimaryAction = value;
        }

        public bool RightHandSecondaryAction {
            get => UsedInput( ref _rightHandSecondaryAction );
            set => _rightHandSecondaryAction = value;
        }

        public bool LeftHandPrimaryAction {
            get => UsedInput( ref _leftHandPrimaryAction );
            set => _leftHandPrimaryAction = value;
        }

        public bool LeftHandSecondaryAction {
            get => UsedInput( ref _leftHandSecondaryAction );
            set => _leftHandSecondaryAction = value;
        }

        public bool TargetLockOrResetCamera {
            get => UsedInput( ref _targetLockOrResetCamera );
            set => _targetLockOrResetCamera = value;
        }

        public bool InteractOrPickup {
            get => UsedInput( ref _interactOrPickup );
            set => _interactOrPickup = value;
        }

        public bool UseItem {
            get => UsedInput( ref _useItem );
            set => _useItem = value;
        }

        public bool Dodge {
            get => UsedInput( ref _dodge );
            set => _dodge = value;
        }

        public bool TwoHandRightHand {
            get => UsedInput( ref _twoHandRightHand );
            set => _twoHandRightHand = value;
        }

        public bool Sprint {
            get => _sprint; // Need to hold it
            set => _sprint = value;
        }

        public bool TwoHandLeftHand {
            get => UsedInput( ref _twoHandLeftHand );
            set => _twoHandLeftHand = value;
        }
        public bool Block {
            get => _block; // Need to hold it
            set => _block = value;
        }

        public bool UsedInput ( ref bool input ) {
            bool temp = input;
            input = false;
            return temp;
        }

    }
}