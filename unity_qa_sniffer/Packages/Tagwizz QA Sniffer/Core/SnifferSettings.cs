using UnityEngine;

namespace TagwizzQASniffer.Core
{
   public class SnifferSettings: ScriptableObject
   {
      public enum InputTypes
      {
         NEW_INPUT,
         OLD_INPUT
      };

      [SerializeField] private InputTypes inputType;
      [SerializeField] private LifeCycle lifeCycle;
      public LifeCycle LifeCycle => lifeCycle;

   }
}
