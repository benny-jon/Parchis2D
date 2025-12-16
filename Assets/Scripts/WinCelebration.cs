using UnityEngine;

public class WinCelebration : MonoBehaviour
{
    [SerializeField] private ParticleSystem confettiPrefab;

    [SerializeField] private int sortingOrder = 3000;

    public void Play(Vector3 spawnPoint)
    {
        if (confettiPrefab == null) return;

        var confetti = Instantiate(confettiPrefab, spawnPoint, Quaternion.identity);

        var renderer = confetti.GetComponent<ParticleSystemRenderer>();
        renderer.sortingLayerID = SortingLayer.NameToID("Animating");
        renderer.sortingOrder = sortingOrder;

        confetti.Play();

        Destroy(confetti.gameObject, confetti.main.duration + confetti.main.startLifetime.constantMax + 1f);
    }
}
