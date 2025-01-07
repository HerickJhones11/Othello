using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SocialPlatforms.Impl;
public class UIManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI topText;

    [SerializeField]
    private TextMeshProUGUI jogador;

    [SerializeField]
    private TextMeshProUGUI blackScoreText;

    [SerializeField]    
    private TextMeshProUGUI whiteScoreText;

    [SerializeField]    
    private TextMeshProUGUI winnerText;

    [SerializeField]
    private Image blackOverlay;

    [SerializeField]
    private RectTransform playAgainButton;

    [SerializeField]
    private TextMeshProUGUI playerText;
    public void SetPlayerText(Player currentPlayer)
    {
        if(currentPlayer == Player.Black)
        {
            topText.text = "Turno do Preto <sprite name=DiscBlackUp>";
        }
        else if(currentPlayer == Player.White)
        {
            topText.text = "Turno do Branco <sprite name=DiscWhiteUp>";
        }
    }

    public void SetPlayerColor(Player currentPlayer)
    {
        if (currentPlayer == Player.Black)
        {
            jogador.text = "Sua cor: <sprite name=DiscBlackUp>";
        }
        else if (currentPlayer == Player.White)
        {
            jogador.text = "Sua cor: <sprite name=DiscWhiteUp>";
        }
    }
    public void SetSkippedText(Player skippedPlayer)
    {
        if(skippedPlayer == Player.Black)
        {
            topText.text = "Preto não pode se mover! <sprite name=DiscBlackUp>";
        }else if(skippedPlayer == Player.White)
        {
            topText.text = "Branco não pode se mover! <sprite name=DiscWhiteUp>";
        }
    }
    public void SetTopText(string message)
    {
        topText.text = message;
    }
    public IEnumerator AnimateTopText()
    {
        topText.transform.LeanScale(Vector3.one * 1.2f, 0.25f).setLoopPingPong(4);
        yield return new WaitForSeconds(2);
    }

    private IEnumerator ScaleDown(RectTransform rect)
    {
        rect.LeanScale(Vector3.zero, 0.2f);
        yield return new WaitForSeconds(0.2f);
        rect.gameObject.SetActive(false);
    }
    private IEnumerator ScaleUp(RectTransform rect)
    {
        rect.gameObject.SetActive(true);
        rect.localScale = Vector3.zero;
        rect.LeanScale(Vector3.one, 0.2f);
        yield return new WaitForSeconds(0.2f);
    }
    
    public IEnumerator ShowLobbyText(string text)
    {
        playerText.text = text;
        playerText.rectTransform.gameObject.SetActive(true);
        yield return ShowOverlay();
        yield return new WaitForSeconds(0.2f);

    }
    public IEnumerator ShowScoreText()
    {
        yield return ScaleDown(topText.rectTransform);
        yield return ScaleUp(blackScoreText.rectTransform);
        yield return ScaleUp(whiteScoreText.rectTransform);
    }

    public void SetBlackScoreText(int score)
    {
        blackScoreText.text = $"<sprite name=DiscBlackUp> {score}";
    }
    public void SetWhiteScoreText(int score)
    {
        whiteScoreText.text = $"<sprite name=DiscWhiteUp> {score}";
    }

    public IEnumerator ShowOverlay()
    {
        blackOverlay.gameObject.SetActive(true);
        blackOverlay.color = Color.clear;
        blackOverlay.rectTransform.LeanAlpha(0.8f, 1);
        yield return new WaitForSeconds(1);
    }
    private IEnumerator HideOverLay()
    {
        blackOverlay.rectTransform.LeanAlpha(0, 1);
        playerText.gameObject.SetActive(false);
        yield return new WaitForSeconds(1);
        blackOverlay.gameObject.SetActive(false);
    }

    private IEnumerator MoveScoreDown()
    {
        blackScoreText.rectTransform.LeanMoveY(0, 0.5f);
        whiteScoreText.rectTransform.LeanMoveY(0, 0.5f);
        yield return new WaitForSeconds(0.5f);
    }

    public void SetWinnerText(Player winner)
    {
        switch(winner)
        {
            case Player.Black:
                winnerText.text = "Preto venceu!";
                break;
            case Player.White:
                winnerText.text = "Branco venceu!";
                break;
            case Player.None:
                winnerText.text = "Empate";
                break;
        }
    }
    public IEnumerator ShowEndScreen()
    {
        yield return ShowOverlay();
        yield return MoveScoreDown();
        yield return ScaleUp(winnerText.rectTransform);
        //yield return ScaleUp(playAgainButton);
    }
    public IEnumerator ShowBlackScreen()
    {
        yield return ShowOverlay();
    }


    public IEnumerator HideEndScreen()
    {
        StartCoroutine(ScaleDown(winnerText.rectTransform));
        StartCoroutine(ScaleDown(blackScoreText.rectTransform));
        StartCoroutine(ScaleDown(whiteScoreText.rectTransform));
        StartCoroutine(ScaleDown(playAgainButton));

        yield return new WaitForSeconds(0.5f);
        yield return HideOverLay();
    }
    public IEnumerator HideBlackScreen()
    {
        yield return new WaitForSeconds(0.5f);
        yield return HideOverLay();
    }
}
