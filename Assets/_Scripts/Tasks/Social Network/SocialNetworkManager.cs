using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SocialNetworkManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TaskManager taskManager;

    [Header("UI References")]
    public TextMeshProUGUI postTextUI;
    //public Image postImageUI;

    [Header("Variables")]
    public float timeBetweenPosts = 30f;

    private List<PostInfo> repeatablePosts = new List<PostInfo>();
    private Queue<PostInfo> uniquePostQueue = new Queue<PostInfo>();

    // :::::::::: MONO METHODS ::::::::::
    private void Start()
    {
        RegeneratePostsFromCompletedTasks();
        StartCoroutine(ShowPostsRoutine());
    }

    private void OnEnable()
    {
        taskManager.TaskSealed += OnTaskSealed;
    }
    private void OnDisable()
    {
        taskManager.TaskSealed -= OnTaskSealed;
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Chose Which Post Show Next
    public PostInfo GetNextPost()
    {
        if (uniquePostQueue.Count > 0)
            return uniquePostQueue.Dequeue();

        if (repeatablePosts.Count > 0)
        {
            int randomIndex = Random.Range(0, repeatablePosts.Count);
            return repeatablePosts[randomIndex];
        }

        return null;
    }

    // ::::: UI Updates
    public void DisplayPost(PostInfo post)
    {
        if (post == null)
        {
            postTextUI.text = "";
        }
        else
        {
            postTextUI.text = post.text;
            //if (post.illustration != null) postImageUI.sprite = post.illustration;
        }
    }

    // ::::: When a Task is Completed
    private void OnTaskSealed(Task completedTask)
    {
        foreach (PostInfo post in completedTask.info.posts)
        {
            if (post.isRepeatable)
                repeatablePosts.Add(post);
            else uniquePostQueue.Enqueue(post);
        }
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Select and Show Next Post
    private IEnumerator ShowPostsRoutine()
    {
        while (true)
        {
            PostInfo nextPost = GetNextPost();
            DisplayPost(nextPost);
            yield return new WaitForSeconds(timeBetweenPosts);
        }
    }

    // ::::: When Loading Game
    private void RegeneratePostsFromCompletedTasks()
    {
        repeatablePosts.Clear();
        uniquePostQueue.Clear();

        List<Task> sealedTasks = TaskDiary.Instance.tasks.Where(t => t.state == TaskState.Sealed).ToList();

        foreach (Task task in sealedTasks)
            foreach (PostInfo post in task.info.posts)
            {
                if (post.isRepeatable)
                    repeatablePosts.Add(post);
                else uniquePostQueue.Enqueue(post);
            }
    }
}
