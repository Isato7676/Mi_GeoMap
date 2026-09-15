using UnityEngine;
using UnityEngine.SceneManagement;

public class InicioSceneController : MonoBehaviour
{
    public void OnBtnIniciarPartidaClicked()
    {
        SceneManager.LoadScene("EscenaJuego");
    }
}