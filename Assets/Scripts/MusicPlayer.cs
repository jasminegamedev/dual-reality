using UnityEngine;

// This game object acts a special singleton object to handle the music player object.
// Basically, the game object won't be destroyed on load if we load into a scene that has the same song playing as whatever is already playing.
// But if we load into a scene with a different song playing, that one will become the new instance.
// This also sets the volume of the main audio source to the correct setting
public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer _instance;

    public static MusicPlayer Instance { get { return _instance; } }

    // If a game object already exists playing the same song we would be playing, destroy this new instance. If it's playing a different song, make this the new main instance, and destroy the old one.
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            if(_instance.GetComponent<AudioSource>().clip != GetComponent<AudioSource>().clip)
            {
                Destroy(_instance.gameObject);
                _instance = this;
                GetComponent<AudioSource>().volume = PlayerPrefs.GetInt("MusicVolume", 15) / 20.0f;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
            GetComponent<AudioSource>().volume = PlayerPrefs.GetInt("MusicVolume", 15) / 20.0f;
        }
    }

    // Set the Volume of the audio Source to the correct setting.
    private void Start()
    {
        GetComponent<AudioSource>().volume = PlayerPrefs.GetInt("MusicVolume", 15) / 20.0f;
    }
}
