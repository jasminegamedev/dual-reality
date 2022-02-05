using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Struct Representing a pair of arrow game objects.
[System.Serializable]
public struct Arrow
{
    public GameObject LeftArrow;
    public GameObject RightArrow;
}


// This component controls the main menu in the game.
public class MenuController : MonoBehaviour
{
    [Tooltip("What Scene to load in to when game start is selected.")]
    public string StartingSceneName;

    [Tooltip("Sound that plays when moving up and down.")]
    public AudioClip JumpSound;
    [Tooltip("Sound that plays when moving left and right.")]
    public AudioClip SwapSound;

    [Tooltip("Text that represents current sound volume.")]
    public Text SoundText;
    [Tooltip("Text that represents current music volume.")]
    public Text MusicText;
    [Tooltip("Reference to the Pallete Swapper component. Usually on the camera.")]
    public PalleteSwapper pSwapper;
    [Tooltip("Reference to the Sound Effect Audio Source.")]
    public AudioSource Sounds;
    [Tooltip("Reference to the Music Audio Source.")]
    public AudioSource Music;

    [Tooltip("List of Arrow Game objects for each menu position.")]
    public List<Arrow> arrows;
    [Tooltip("Reference to the player gameobject. Menu works differently depending on if there is a player or not.")]
    public GameObject player;
    [Tooltip("Reference to the canvas gameobject.")]
    public GameObject canvas;

    // Cache for the previous horizontal axis value, so we can know when that changes.
    private float prevHorizontal;
    // Cache for the previous vertical axis value, so we can know when that changes.
    private float prevVertical;

    // Current vertical menu position.
    private int vPosition = 0;
    // Current selected pallete.
    private int palette = 0;
    // Current Sound Volume.
    private int soundVolume = 15;
    // Current Music Volume.
    private int musicVolume = 15;

    void Start()
    {
        if (player != null)
        {
            Sounds = player.GetComponent<AudioSource>();
        }
        else if(Sounds == null)
        {
            Sounds = GetComponent<AudioSource>();
        }

        ResetState();

        prevHorizontal = Input.GetAxis("Horizontal");
        prevVertical = Input.GetAxis("Vertical");
    }

    // Resets the state of the menu to the correct starting values
    public void ResetState()
    {
        vPosition = 0;
        palette = PlayerPrefs.GetInt("Pallete", 0);
        soundVolume = PlayerPrefs.GetInt("SoundVolume", 15);
        musicVolume = PlayerPrefs.GetInt("MusicVolume", 15);
        SoundText.text = BuildVolumeText(soundVolume);
        MusicText.text = BuildVolumeText(musicVolume);
        Sounds.volume = soundVolume / 20.0f;
        Music = MusicPlayer.Instance.GetComponent<AudioSource>();
        Music.volume = musicVolume / 20.0f;

        SetArrows(0);
    }

    // Sets the correct arrows to be active or hidden.
    private void SetArrows(int position)
    {
        for (int i = 0; i < arrows.Count; i++)
        {
            arrows[i].LeftArrow.SetActive(i == position);
            arrows[i].RightArrow.SetActive(i == position);
        }
    }

    void Update()
    {
        bool pressedDown = (prevVertical > -0.2f && Input.GetAxis("Vertical") <= -0.2f);
        bool pressedUp = (prevVertical < 0.2f && Input.GetAxis("Vertical") >= 0.2f);

        if (pressedDown)
        {
            vPosition = (vPosition + 1) % 4;
            SetArrows(vPosition);
            Sounds.PlayOneShot(JumpSound);
        }
        else if (pressedUp)
        {
            vPosition = (4 + (vPosition - 1)) % 4;
            SetArrows(vPosition);
            Sounds.PlayOneShot(JumpSound);
        }

        switch(vPosition)
        {
            case 0:
                UpdateCheckStartGame();
                break;
            case 1:
                UpdatePallete();
                break;
            case 2:
                UpdateSoundVolume();
                break;
            case 3:
                UpdateMusicVolume();
                break;
        }

        prevHorizontal = Input.GetAxis("Horizontal");
        prevVertical = Input.GetAxis("Vertical");
    }

    // Function for starting or continuing game when the player presses a button while it's selected.
    // If there is a player, it means we are in game, and should just contine. If there isn't a player, start the game.
    private void UpdateCheckStartGame()
    {
        if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump"))
        {
            if (player == null)
            {
                SceneManager.LoadScene(StartingSceneName);
            }
            else
            {
                canvas.SetActive(false);
                player.GetComponent<Rigidbody2D>().simulated = true;
            }
        }
    }

    // Function for updating color pallete when it changes
    private void UpdatePallete()
    {
        bool pressedLeft = (prevHorizontal > -0.2f && Input.GetAxis("Horizontal") <= -0.2f);
        bool pressedRight = (prevHorizontal < 0.2f && Input.GetAxis("Horizontal") >= 0.2f);

        if (pressedRight)
        {
            palette += 1;
            if (palette >= pSwapper.materials.Count)
            {
                palette = 0;
            }
            PlayerPrefs.SetInt("Pallete", palette);
            Sounds.PlayOneShot(SwapSound);
        }
        else if (pressedLeft)
        {
            palette -= 1;
            if (palette < 0)
            {
                palette = pSwapper.materials.Count - 1;
            }
            PlayerPrefs.SetInt("Pallete", palette);
            Sounds.PlayOneShot(SwapSound);
        }
    }

    // Function for updating sound effects volume when it changes
    private void UpdateSoundVolume()
    {
        bool pressedLeft = (prevHorizontal > -0.2f && Input.GetAxis("Horizontal") <= -0.2f);
        bool pressedRight = (prevHorizontal < 0.2f && Input.GetAxis("Horizontal") >= 0.2f);

        if (pressedRight)
        {
            soundVolume += 1;
            soundVolume = Mathf.Min(soundVolume, 20);
            SoundText.text = BuildVolumeText(soundVolume);
            PlayerPrefs.SetInt("SoundVolume", soundVolume);
            Sounds.volume = soundVolume / 20.0f;
            Sounds.PlayOneShot(SwapSound);
        }
        else if (pressedLeft)
        {
            soundVolume -= 1;
            soundVolume = Mathf.Max(soundVolume, 0);
            SoundText.text = BuildVolumeText(soundVolume);
            PlayerPrefs.SetInt("SoundVolume", soundVolume);
            Sounds.volume = soundVolume / 20.0f;
            Sounds.PlayOneShot(SwapSound);
        }
    }

    // Function for updating music volume when it changes
    private void UpdateMusicVolume()
    {
        bool pressedLeft = (prevHorizontal > -0.2f && Input.GetAxis("Horizontal") <= -0.2f);
        bool pressedRight = (prevHorizontal < 0.2f && Input.GetAxis("Horizontal") >= 0.2f);

        if (pressedRight)
        {
            musicVolume += 1;
            musicVolume = Mathf.Min(musicVolume, 20);
            MusicText.text = BuildVolumeText(musicVolume);
            PlayerPrefs.SetInt("MusicVolume", musicVolume);
            Sounds.PlayOneShot(SwapSound);
            Music.volume = musicVolume / 20.0f;
        }
        else if (pressedLeft)
        {
            musicVolume -= 1;
            musicVolume = Mathf.Max(musicVolume, 0);
            MusicText.text = BuildVolumeText(musicVolume);
            PlayerPrefs.SetInt("MusicVolume", musicVolume);
            Sounds.PlayOneShot(SwapSound);
            Music.volume = musicVolume / 20.0f;
        }
    }

    // Build a string representing the volume.
    private string BuildVolumeText(int length)
    {
        string text = " ";
        for(int i = 0; i < length; i++)
        {
            text += "I";
        }
        return text;
    }
}
