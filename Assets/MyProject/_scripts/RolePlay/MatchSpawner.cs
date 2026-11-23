
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject aiPrefab;

    [Header("Pontos de Spawn")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Skins Disponíveis")]
    [SerializeField] private List<CharacterSkin> availableSkins;

    [Header("Organização")]
    [SerializeField] private Transform characterParent;

    private bool isInitializing;

    public bool IsInitializing => isInitializing;

    public bool BeginMatch(
        int aiAmount,
        CharacterSkin selectedSkin,
        Action<bool> onFinished)
    {
        if (isInitializing)
            return false;

        if (BombManager.Instance == null)
        {
            Debug.LogError("BombManager não encontrado.");
            return false;
        }

        if (BombManager.Instance.Phase !=
            BombManager.MatchPhase.WaitingToStart)
        {
            Debug.LogWarning("A partida já foi iniciada.");
            return false;
        }

        if (aiAmount < 2 || aiAmount > 8)
        {
            Debug.LogError("Quantidade de IAs inválida.");
            return false;
        }

        if (selectedSkin == null ||
            string.IsNullOrEmpty(selectedSkin.skinID))
        {
            Debug.LogError("Skin selecionada inválida.");
            return false;
        }

        if (playerPrefab == null || aiPrefab == null)
        {
            Debug.LogError("Prefabs não configurados.");
            return false;
        }

        if (spawnPoints == null ||
            spawnPoints.Length < aiAmount + 1)
        {
            Debug.LogError("Pontos de spawn insuficientes.");
            return false;
        }

        foreach (Transform point in spawnPoints)
        {
            if (point == null)
            {
                Debug.LogError("Existe um spawn vazio.");
                return false;
            }
        }

        if (playerPrefab.GetComponent<BombCarrier>() == null ||
            aiPrefab.GetComponent<BombCarrier>() == null)
        {
            Debug.LogError("Prefab sem BombCarrier.");
            return false;
        }

        if (playerPrefab.GetComponent<PlayerElimination>() == null ||
            aiPrefab.GetComponent<PlayerElimination>() == null)
        {
            Debug.LogError("Prefab sem PlayerElimination.");
            return false;
        }

        List<CharacterSkin> skinsForAI =
            GetAvailableSkins(selectedSkin);

        if (skinsForAI.Count < aiAmount)
        {
            Debug.LogError(
                "Não há skins suficientes para todas as IAs."
            );
            return false;
        }

        isInitializing = true;

        StartCoroutine(
            SpawnAndStart(
                aiAmount,
                selectedSkin,
                skinsForAI,
                onFinished
            )
        );

        return true;
    }

    private List<CharacterSkin> GetAvailableSkins(
        CharacterSkin selectedSkin)
    {
        List<CharacterSkin> result =
            new List<CharacterSkin>();

        HashSet<string> usedIDs =
            new HashSet<string>();

        usedIDs.Add(selectedSkin.skinID);

        foreach (CharacterSkin skin in availableSkins)
        {
            if (skin == null ||
                string.IsNullOrEmpty(skin.skinID))
                continue;

            if (usedIDs.Add(skin.skinID))
                result.Add(skin);
        }

        return result;
    }

    private IEnumerator SpawnAndStart(
        int aiAmount,
        CharacterSkin selectedSkin,
        List<CharacterSkin> aiSkins,
        Action<bool> onFinished)
    {
        // Embaralha as skins das IAs.
        for (int i = aiSkins.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(
                0, i + 1
            );

            CharacterSkin temp = aiSkins[i];
            aiSkins[i] = aiSkins[randomIndex];
            aiSkins[randomIndex] = temp;
        }

        // Jogador humano.
        GameObject player = Instantiate(
            playerPrefab,
            spawnPoints[0].position,
            Quaternion.identity,
            characterParent
        );

        player.name = "Player";

        ApplySkin(player, selectedSkin);

        // Inteligências artificiais.
        for (int i = 0; i < aiAmount; i++)
        {
            GameObject ai = Instantiate(
                aiPrefab,
                spawnPoints[i + 1].position,
                Quaternion.identity,
                characterParent
            );

            ai.name = "IA " + (i + 1);

            ApplySkin(ai, aiSkins[i]);
        }

        // Aguarda os Start() dos BombCarriers.
        yield return null;

        BombManager manager = BombManager.Instance;

        bool success =
            manager != null &&
            manager.ActivePlayers.Count == aiAmount + 1;

        if (success)
        {
            manager.StartMatch();

            success =
                manager.Phase ==
                BombManager.MatchPhase.Playing;
        }
        else
        {
            Debug.LogError(
                "Falha no registro dos participantes."
            );
        }

        isInitializing = false;
        onFinished?.Invoke(success);
    }

    private void ApplySkin(
        GameObject character,
        CharacterSkin skin)
    {
        BombCarrier carrier =
            character.GetComponent<BombCarrier>();

        if (carrier == null)
            return;

        carrier.SetAppearance(
            skin.characterSprite,
            skin.characterColor
        );
    }
}
