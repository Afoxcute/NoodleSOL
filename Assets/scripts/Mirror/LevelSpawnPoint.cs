using UnityEngine;

public class LevelSpawnPoint : MonoBehaviour
{
    private void Awake() => LevelSpawnSystem.AddSpawnPoint(transform);
    private void OnDestroy() => LevelSpawnSystem.RemoveSpawnPoint(transform);

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.blue;
    //     Gizmos.DrawSphere(transform.position, 1f);
    //     Gizmos.color = Color.green;
    //     Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2);
    // }
}
