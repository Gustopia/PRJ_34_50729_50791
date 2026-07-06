/*
 * Este script cria um loading screen, com animação de entrada e saída durante a transição 
 * entre cenas no jogo, incluindo uma barra de progresso durante o carregamento.
 * A animação de transição é definida por componetes SceneTransition (e.g. fade in/out, slide left, etc.)
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;

// #my_code
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public Slider progressBar;
    public GameObject transitionsContainer;

    private SceneTransition[] transitions;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Guardar todos os componentes SceneTransition filhos do transitionsContainer
        transitions = transitionsContainer.GetComponentsInChildren<SceneTransition>();
    }

    public void LoadScene(string sceneName, string transitionName)
    {
        Time.timeScale = 1f; // Jogo na velocidade normal
        PauseMenu.IsPaused = false;
        StartCoroutine(LoadSceneAsync(sceneName, transitionName));
    }

    private IEnumerator LoadSceneAsync(string sceneName, string transitionName)
    {
        // Encontrar a transição correspondente pelo nome
        SceneTransition transition = transitions.First(t => t.name == transitionName);

        // Carregar a cena de forma assíncrona, mas não ativá-la imediatamente
        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        yield return transition.AnimateTransitionIn();
       
        progressBar.gameObject.SetActive(true);

        do
        {
            progressBar.value = scene.progress;
            yield return null;
        }
        while (scene.progress < 0.9f);

        scene.allowSceneActivation = true;
        progressBar.gameObject.SetActive(false);

        yield return transition.AnimateTransitionOut();
    }
}