using UnityEngine;

namespace RPGProject.Character
{
    /// <summary>
    /// Dados de configuracao para movimento top-down de personagens.
    /// ScriptableObjects deixam o balanceamento editavel sem alterar codigo.
    /// </summary>
    [CreateAssetMenu(
        fileName = "CharacterMovementSettings",
        menuName = "RPG Project/Character/Movement Settings")]
    public sealed class CharacterMovementSettings : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Velocidade maxima do personagem em unidades por segundo.")]
        [SerializeField]
        [Min(0f)]
        private float moveSpeed = 4f;

        public float MoveSpeed => moveSpeed;
    }
}
