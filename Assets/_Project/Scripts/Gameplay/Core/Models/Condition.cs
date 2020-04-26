
namespace DistantMeadows.Core.Models
{
    public class Condition<Type>
    {
        public Type condition;

        public Condition(Type condition)
        {
            this.condition = condition;
        }
    }
}
