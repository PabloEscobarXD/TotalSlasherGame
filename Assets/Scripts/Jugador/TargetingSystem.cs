using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class TargetingSystem : MonoBehaviour
{
    public float targetingRange = 5f;   // Largo del cono
    public float coneAngle = 45f;       // Apertura del cono en grados
    public LayerMask enemyLayer;

    public Transform GetNearestEnemy()
    {
        // Buscar todos los enemigos dentro de la esfera de rango
        Collider[] hits = Physics.OverlapSphere(transform.position, targetingRange, enemyLayer);

        if (hits.Length == 0) return null;

        // Filtrar solo los que estén dentro del cono
        var enemiesInCone = hits
            .Where(h =>
            {
                Vector3 dirToEnemy = (h.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dirToEnemy);
                return angle <= coneAngle / 2f; // dentro del cono
            });

        if (!enemiesInCone.Any()) return null;

        // Ordenar por distancia
        Collider nearest = enemiesInCone
            .OrderBy(h => Vector3.Distance(transform.position, h.transform.position))
            .FirstOrDefault();

        return nearest?.transform;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Dibuja líneas para visualizar el cono
        Vector3 forward = transform.forward * targetingRange;
        Quaternion leftRot = Quaternion.Euler(0, -coneAngle / 2f, 0);
        Quaternion rightRot = Quaternion.Euler(0, coneAngle / 2f, 0);

        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.DrawRay(transform.position, forward);
        Gizmos.DrawRay(transform.position, leftDir);
        Gizmos.DrawRay(transform.position, rightDir);

        // Opcional: dibujar la base del cono como arco
        int segments = 20;
        Vector3 prevPoint = transform.position + leftDir;
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 dir = Quaternion.Euler(0, -coneAngle / 2f + coneAngle * t, 0) * forward;
            Vector3 point = transform.position + dir;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }

    public List<Transform> GetEnemiesInRange(int maxCount, float range)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range, enemyLayer);
        return hits
            .OrderBy(h => Vector3.Distance(transform.position, h.transform.position))
            .Take(maxCount)
            .Select(h => h.transform)
            .ToList();
    }

    public Transform GetNearestEnemyInDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f)
            direction = transform.forward; // fallback si no hay input

        Collider[] hits = Physics.OverlapSphere(transform.position, targetingRange, enemyLayer);
        if (hits.Length == 0) return null;

        var enemiesInCone = hits.Where(h =>
        {
            Vector3 dirToEnemy = (h.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(direction.normalized, dirToEnemy);
            return angle <= coneAngle / 2f;
        });

        if (!enemiesInCone.Any()) return null;

        return enemiesInCone
            .OrderBy(h => Vector3.Distance(transform.position, h.transform.position))
            .First().transform;
    }
}
