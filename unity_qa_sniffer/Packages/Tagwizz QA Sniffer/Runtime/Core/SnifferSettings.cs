using TagwizzQASniffer.Core;
using TagwizzQASniffer.Core.InputSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace TagwizzQASniffer.Core
{
   public class SnifferSettings: ScriptableObject
   {
      public enum InputSystemType
      {
         NEW_INPUT,
         OLD_INPUT
      };

      [SerializeField] private InputSystemType inputSystemType;
      [SerializeField] private LifeCycle lifeCycle;
      public LifeCycle LifeCycle => lifeCycle;
      public InputSystemType InputSystem => inputSystemType;

   }
}
