using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum States
{
    CanMove,
    CantMove
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public BoxCollider2D collider;
    public GameObject token1, token2;
    public int Size = 3;
    public int[,] Matrix;
    [SerializeField] private States state = States.CanMove;
    public Camera camera;
    public AudioSource audio;
    void Start()
    {
        Instance = this;
        Matrix = new int[Size, Size];
        Calculs.CalculateDistances(collider, Size);
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Matrix[i, j] = 0; // 0: desocupat, 1: fitxa jugador 1, -1: fitxa IA;
            }
        }
    }
    private void Update()
    {
        if (state == States.CanMove)
        {
            Vector3 m = Input.mousePosition;
            m.z = 10f;
            Vector3 mousepos = camera.ScreenToWorldPoint(m);
            if (Input.GetMouseButtonDown(0))
            {
                if (Calculs.CheckIfValidClick((Vector2)mousepos, Matrix))
                {
                    audio.PlayOneShot(audio.clip);
                    state = States.CantMove;
                    if(Calculs.EvaluateWin(Matrix)==2)
                        StartCoroutine(WaitingABit());
                }
            }
        }
    }
    private IEnumerator WaitingABit()
    {
        yield return new WaitForSeconds(1f);
        RandomAI();
    }
    public void RandomAI()
    {
        int bestVal = int.MinValue;
        int[] bestMove = new int[] { 0, 0 };

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (Matrix[i, j] == 0)
                {
                    Matrix[i, j] = -1;
                    int currentVal = Minimax(Matrix, 0, false);
                    Matrix[i, j] = 0;

                    if (currentVal > bestVal)
                    {
                        bestVal = currentVal;
                        bestMove = new int[] { i, j };
                    }
                }
            }
        }


        audio.PlayOneShot(audio.clip);
        DoMove(bestMove[0], bestMove[1], -1);
        state = States.CanMove;
        // DoMove(x, y, -1);
        // state = States.CanMove;
    }

    public int Minimax(int[,] tempMatrix, int depth, bool isMaximizing)
    {
        int score = Calculs.EvaluateWin(tempMatrix);

        if (score == -1) return 10 - depth;

        if (score == 1) return depth - 10;

        if (score == 0) return 0;

        if (isMaximizing)
        {
            int best = int.MinValue;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (tempMatrix[i, j] == 0)
                    {
                        tempMatrix[i, j] = -1;
                        best = Mathf.Max(best, Minimax(tempMatrix, depth + 1, false));
                        tempMatrix[i, j] = 0;
                    }
                }
            }
            return best;
        }
        else
        {
            int best = int.MaxValue;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (tempMatrix[i, j] == 0)
                    {
                        tempMatrix[i, j] = 1;
                        best = Mathf.Min(best, Minimax(tempMatrix, depth + 1, true));
                        tempMatrix[i, j] = 0;
                    }
                }
            }
            return best;
        }
    }


    public void DoMove(int x, int y, int team)
    {
        Matrix[x, y] = team;
        if (team == 1)
            Instantiate(token1, Calculs.CalculatePoint(x, y), Quaternion.identity);
        else
            Instantiate(token2, Calculs.CalculatePoint(x, y), Quaternion.identity);
        int result = Calculs.EvaluateWin(Matrix);
        switch (result)
        {
            case 0:
                Debug.Log("Draw");
                break;
            case 1:
                Debug.Log("You Win");
                break;
            case -1:
                Debug.Log("You Lose");
                break;
            case 2:
                if(state == States.CantMove)
                    state = States.CanMove;
                Debug.Log("More!");
                break;
        }
    }
}
