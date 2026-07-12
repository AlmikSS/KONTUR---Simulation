using UnityEngine;

namespace Core.Input
{
    public struct InputSnapshot
    {
        public readonly InputContext Context;
        public readonly Vector2 MoveInput;
        public readonly Vector2 LookInput;
        public readonly bool InteractInput;
        public readonly bool SecondInteractInput;
        public readonly bool CrouchInput;
        public readonly bool OpenConsole;
        public readonly bool JumpInput;
        public readonly bool Slot1Input;
        public readonly bool Slot2Input;
        public readonly bool Slot3Input;
        public readonly bool Slot4Input;

        public InputSnapshot(InputContext context, Vector2 moveInput, Vector2 lookInput, bool interactInput, bool openConsole, bool jumpInput, bool secondInteractInput, bool crouchInput, bool slot1Input, bool slot2Input, bool slot3Input, bool slot4Input)
        {
            Context = context;
            MoveInput = moveInput;
            LookInput = lookInput;
            InteractInput = interactInput;
            OpenConsole = openConsole;
            JumpInput = jumpInput;
            SecondInteractInput = secondInteractInput;
            CrouchInput = crouchInput;
            Slot1Input = slot1Input;
            Slot2Input = slot2Input;
            Slot3Input = slot3Input;
            Slot4Input = slot4Input;
        }
    }
}