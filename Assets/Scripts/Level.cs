using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    private const float CAMERA_ORTHO_SIZE = 50f;
    private const float PIPE_WIDTH = 7.8f;
    private const float PIPEHEAD_WIDTH = 3.75f;
    public static float PIPE_MS = 30f;
    private const float SCREEN_LEFT_SIDE = -125;
    private const float SCREEN_RIGHT_SIDE = 125;
    private float gapSize;
    private List<Pipe> PipeList;
    private float pipeSpawnTimer;
    private float pipeSpawnTimerMax;
    private int pipesSpawned;
    private float SPRINT_DURATION = 3;
    private float sprint_timer;
    private bool sprint = false;
    private GameState gamestate;

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }
    
    private enum GameState
    {
        Playing,
        DedBerd
    }

    private void Awake() 
    {
        PipeList = new List<Pipe>();
        pipeSpawnTimerMax = 1.4f;
        gapSize = 50f;
        gamestate = GameState.Playing;
    }
    private void Start()
    {  
        Berd.GetInstance().OnDied +=Berd_OnDied;
    }

    private void Berd_OnDied(object sender, System.EventArgs a)
    {
        if(sprint)
        {
            ResetSprint();
        }
        
        gamestate = GameState.DedBerd;
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    private void Update() 
    {
        if (gamestate == GameState.Playing)
        {
            HandlePipeMovement();
            HandlePipeSpawning(); 
        }
        if(Input.GetMouseButton(1) && !sprint)
        {
            sprint = true;
            Debug.Log(PIPE_MS);
            PIPE_MS *= 2f;
            Debug.Log(PIPE_MS);
            pipeSpawnTimerMax /= 2f;
            sprint_timer = SPRINT_DURATION;
        }

        if(sprint)
        {
            sprint_timer -= Time.deltaTime;
            if(sprint_timer <= 0)
            {
                ResetSprint();
            }
        }
        
    }

    private void ResetSprint()
    {
        
        sprint = false;
        Debug.Log(PIPE_MS);
        PIPE_MS /= 2f;
        Debug.Log(PIPE_MS);
        pipeSpawnTimerMax *= 2f;
        sprint_timer = SPRINT_DURATION;
    }

    private void HandlePipeMovement()
    {
        for (int i =0; i<PipeList.Count; i++)
        {
            Pipe pipe = PipeList[i];
            pipe.Move();

            if (pipe.getXPos() < SCREEN_LEFT_SIDE)
            {
                pipe.SelfDestruct();
                PipeList.Remove(pipe);
                i--;
            }

        }
    }

    
    private void HandlePipeSpawning()
    {
        pipeSpawnTimer -= Time.deltaTime;

        if (pipeSpawnTimer < 0 )
        {

            pipeSpawnTimer = pipeSpawnTimerMax;


            float heightEdgeLimit = 10f;
            float minheight = gapSize*.5f + heightEdgeLimit;
            //float minheight = gapSize*.5f;
            float totalHeight = CAMERA_ORTHO_SIZE*2f;
            float maxHeight = totalHeight - gapSize*.5f -heightEdgeLimit;
            //float maxHeight = totalHeight - gapSize*.5f;
            float height = Random.Range(minheight,maxHeight);
            
            CreateGapPipes(height,gapSize,SCREEN_RIGHT_SIDE);
            
        }
    }

    private void CreateGapPipes(float gapY,float gapSize,float xPos)
    {
        CreatePipe(xPos, gapY - gapSize*.5f, false);
        CreatePipe(xPos, (CAMERA_ORTHO_SIZE*2f) - gapY - gapSize*.5f, true);
        pipesSpawned ++;
        if(!sprint)
        {
            SetDifficulty(GetDifficulty());
        }
        
    }

    private Difficulty GetDifficulty ()
    {
        if (pipesSpawned >30) 
        {
            return Difficulty.Hard;
        }
        if (pipesSpawned >10) 
        {
            return Difficulty.Medium;
        }
        
        return Difficulty.Easy;
    }
    private void SetDifficulty(Difficulty difficulty)
    {
        if(difficulty == Difficulty.Easy)
        {
            gapSize = 50;
            pipeSpawnTimerMax = 1.4f;
        }
        if(difficulty == Difficulty.Medium)
        {
            gapSize = 40;
            pipeSpawnTimerMax = 1.2f;
        }
        if(difficulty == Difficulty.Hard)
        {
            gapSize = 30;
            pipeSpawnTimerMax = 1f;
        }

    }
    private void CreatePipe(float xpos, float height, bool reverse)
    {

        Transform pipeHead = Instantiate(GameAssets.GetInstance().pfPipeHead);
        Transform pipeBody = Instantiate(GameAssets.GetInstance().pfPipeBody);

        float pipeBodyYpos = 0;
        float pipeHeadYpos = 0;

        if(reverse)
        {
            pipeHeadYpos = CAMERA_ORTHO_SIZE-height + (PIPEHEAD_WIDTH* .5f);
            pipeBodyYpos = CAMERA_ORTHO_SIZE;
            pipeBody.localScale= new Vector3(1,-1,1);
        }
        else if(!reverse)
        {
            pipeHeadYpos = -CAMERA_ORTHO_SIZE+height - (PIPEHEAD_WIDTH* .5f);
            pipeBodyYpos = -CAMERA_ORTHO_SIZE;
        }

          
        
        pipeHead.position = new Vector3(xpos,pipeHeadYpos);
        pipeBody.position = new Vector3(xpos, pipeBodyYpos);




        SpriteRenderer pipeBodySpriteRenderer = pipeBody.GetComponent<SpriteRenderer>();
        pipeBodySpriteRenderer.size = new Vector2(PIPE_WIDTH, height);



        BoxCollider2D pipeBodyBoxCollider = pipeBody.GetComponent<BoxCollider2D>();
        pipeBodyBoxCollider.size = new Vector2(PIPE_WIDTH, height);
        pipeBodyBoxCollider.offset = new Vector2(0f, height * .5f);


        Pipe pipe = new Pipe(pipeHead,pipeBody);
        PipeList.Add(pipe);
    }
    
    private class Pipe
    {
        private Transform PipeHeadTransform;
        private Transform PipeBodyTransform;

        public Pipe(Transform PipeHeadTransform,Transform PipeBodyTransform )
        {
            this.PipeBodyTransform = PipeBodyTransform;
            this.PipeHeadTransform = PipeHeadTransform;
        }


        public void Move()
        {
            PipeHeadTransform.position += new Vector3(-1,0,0) * Level.PIPE_MS* Time.deltaTime;
            PipeBodyTransform.position += new Vector3(-1,0,0) * Level.PIPE_MS* Time.deltaTime;
        }

        public void SelfDestruct()
        {
            Destroy(PipeBodyTransform.gameObject);
            Destroy(PipeHeadTransform.gameObject);
        }

        public float getXPos()
        {
            return PipeHeadTransform.position.x;
        }
    }
    
}
