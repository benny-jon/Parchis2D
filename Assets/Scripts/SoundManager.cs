using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip diceRollClip;
    [SerializeField] private AudioClip pieceClickedClip;
    [SerializeField] private AudioClip pieceMoveEndClip;
    [SerializeField] private AudioClip captureClip;
    [SerializeField] private AudioClip pieceToStartClip;
    [SerializeField] private AudioClip homeClip;
    [SerializeField] private AudioClip playerWinClip;
    [SerializeField] private AudioClip uiClickClip;


    private AudioSource sfxSource;

    private void Awake()
    {
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayDiceRoll()           => PlayClip(diceRollClip);
    public void PlayPieceClicked()     => PlayClip(pieceClickedClip);
    public void PlayPieceMoveEnd()       => PlayClip(pieceMoveEndClip);
    public void PlayCapture()            => PlayClip(captureClip);
    public void PlayPieceToStart()       => PlayClip(pieceToStartClip);
    public void PlayHome()               => PlayClip(homeClip);
    public void PlayPlayerWin()          => PlayClip(playerWinClip);
    public void PlayUIClick()            => PlayClip(uiClickClip);
}
