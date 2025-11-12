namespace Scripts.EventBus.Events
{
    public class PlayerGroundedStateChangedEvent
    {
        public bool IsGrounded;

        public PlayerGroundedStateChangedEvent(bool isGrounded)
        {
            IsGrounded = isGrounded;
        }
    }
}