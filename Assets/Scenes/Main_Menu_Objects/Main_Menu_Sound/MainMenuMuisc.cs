using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    [Header("Trilha sonora do Menu")]
    [SerializeField] private AudioClip musicaMenu;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.7f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = musicaMenu;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (musicaMenu != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Nenhuma música foi adicionada ao MainMenuMusic.");
        }
    }
}