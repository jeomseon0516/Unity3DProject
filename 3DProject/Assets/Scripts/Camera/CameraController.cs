using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public partial class CameraController : MonoBehaviour
{
    private const float ANGLE_SPEED_MAX = 80.0f;
    private const float CORRECTION_DISTANCE = 1.0f;
    private const float MAX_DISTANCE = 3.8f;
    private const float MIN_DISTANCE = 1.5f;
    private const float CAMERA_SPEED = 10.0f;
    private const float Y_ANGLE_MAX = 64.0f;
    private const float WHEEL_SPEED = 800.0f;
    private const float DRAG_SPEED = 15.0f;
    private const int X = 0;
    private const int Y = 1;

    [SerializeField, Range(0.0f, 360.0f)] private float[] _cameraAngle;
    [SerializeField, Range(MIN_DISTANCE, MAX_DISTANCE)] private float _distance;
    [SerializeField] private Vector3 _lookOffset;
    [SerializeField] private Vector3 _offset;

    private Vector3 _beforeMousePosition;
    private Dictionary<string, Dictionary<string, IEnumerator>> _coroutineMap = new Dictionary<string, Dictionary<string, IEnumerator>>();

    [field:SerializeField] public GameObject TargetObject { get; set; }

    private void Start()
    {
        _beforeMousePosition = Input.mousePosition;
        _cameraAngle = new float[] { 0.0f, 0.0f };
        _lookOffset = new Vector3(0.0f, 1.5f, 0.0f);
        _offset = new Vector3(0.0f, 1.0f, 0.0f);
        _distance = 5.0f;
    }
    private void Update()
    {
        controlMouseWheel();
        controlMouseDrag();
        focusingCamera();
        checkBlockObject();
    }

    private void focusingCamera()
    {
        if (ReferenceEquals(TargetObject, null) || !TargetObject) return;

        _cameraAngle[Y] = _cameraAngle[Y] % 360.0f;

        if      (_cameraAngle[Y] < -Y_ANGLE_MAX) // 특정 각도 이상으로 넘어가면 카메라가 휙휙 돌아가서 눈 아프므로 각도 제한을 준다.
            _cameraAngle[Y] = -Y_ANGLE_MAX;
        else if (_cameraAngle[Y] >  Y_ANGLE_MAX)
            _cameraAngle[Y] =  Y_ANGLE_MAX;

        Vector3 cameraDirection = Quaternion.Euler(_cameraAngle[Y], _cameraAngle[X], 0.0f) * Vector3.back; // 카메라가 위치할 방향 (시점 아님 카메라 트랜스폼)
        Vector3 cameraPosition = TargetObject.transform.position + _offset + cameraDirection * _distance; // 타겟의 위치로부터 + 오프셋 + 방향 * 카메라가 타겟으로부터 얼만큼 떨어질 건지의 거리

        // 타겟과의 거리 구하기 y좌표를 뺀 2차원 상에서의 거리 구하기
        float targetDistance = CustomMath.GetDistance(TargetObject.transform.position.z, transform.position.z, 
                                                      TargetObject.transform.position.x, transform.position.x);

        // 캐릭터가 카메라를 따라잡는 현상 방지
        if (targetDistance < CORRECTION_DISTANCE)
            _distance += CORRECTION_DISTANCE - targetDistance;

        // 카메라가 이동할 위치와 현재 카메라의 위치 거리 구하기
        float   distance  = Vector3.Distance(cameraPosition, transform.position);
        // 방향 구하기
        Vector3 direction = (cameraPosition - transform.position).normalized;

        transform.position += direction * distance * Time.deltaTime * CAMERA_SPEED;

        // 바라볼 방향의 오프셋을 더한 값의 정규화를 시키고
        Vector3 targetDirection = ((TargetObject.transform.position + _lookOffset) - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(targetDirection); // 방향으로 카메라 로테이션, 타겟 오브젝트를 바라본다.
    }
    private void controlMouseWheel()
    {
        float wheelInput = WHEEL_SPEED * -Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime;

        _distance += wheelInput;

        if      (_distance < MIN_DISTANCE)
            _distance = MIN_DISTANCE;
        else if (_distance > MAX_DISTANCE)
            _distance = MAX_DISTANCE;
    }
    private void controlMouseDrag()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1)) // 갑자기 화면이 크게 돌아가는 현상을 방지
            _beforeMousePosition = Input.mousePosition;

        if (Input.GetKey(KeyCode.Mouse1))
        {
            Vector3 interval = Input.mousePosition - _beforeMousePosition;

            if (Mathf.Abs(interval.x) > ANGLE_SPEED_MAX)
                interval.x = interval.x < 0 ? -ANGLE_SPEED_MAX : ANGLE_SPEED_MAX;
            if (Mathf.Abs(interval.y) > ANGLE_SPEED_MAX)
                interval.y = interval.y < 0 ? -ANGLE_SPEED_MAX : ANGLE_SPEED_MAX;

            _cameraAngle[X] +=  interval.x * DRAG_SPEED * Time.deltaTime;
            _cameraAngle[Y] += -interval.y * DRAG_SPEED * Time.deltaTime;

            _beforeMousePosition = Input.mousePosition;
        }
    }
}

public partial class CameraController : MonoBehaviour
{
    private string SHADER_PATH = "Legacy Shaders/Transparent/Specular";

    [SerializeField] private LayerMask _layerMask;
    private List<Transform> _objects = new List<Transform>();

    private void checkBlockObject()
    {
        float distance = Vector3.Distance(transform.position, TargetObject.transform.position);

        Debug.DrawRay(transform.position, transform.forward * distance, Color.red);

        Ray ray = new Ray(transform.position, transform.forward);

        RaycastHit[] hits = Physics.RaycastAll(ray, distance);
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.1f);

        foreach (RaycastHit hit in hits)
            checkKeepObjectFromTransform(hit.transform);

        foreach (Collider collider in colliders)
            checkKeepObjectFromTransform(collider.transform);

        for (int i = 0; i < _objects.Count;)
        {
            if (!hits.Any(hit =>      hit.transform.Equals(_objects[i])) &&
                !colliders.Any(hit => hit.transform.Equals(_objects[i])) &&
                _objects[i].TryGetComponent(out Renderer renderer))
            {
                StartCoroutine(setFadeIn(renderer, 1.0f));
                _objects.Remove(_objects[i]);
            }
            else
                ++i;
        }
    }
    private IEnumerator setFadeOut(Renderer renderer, float targetValue)
    {
        if (ReferenceEquals(renderer, null) || !renderer) yield break;

        renderer.material.shader = Shader.Find(SHADER_PATH);
        Color color = renderer.material.color;

        float interval = Mathf.Infinity;

        while (interval > 0.01f)
        {
            color.a = Mathf.Lerp(color.a, targetValue, Time.deltaTime * 10.0f);
            renderer.material.color = color;

            interval = Mathf.Abs(color.a - targetValue);

            yield return null;
        }

        color.a = targetValue;
        renderer.material.color = color;
    }

    private IEnumerator setFadeIn(Renderer renderer, float targetValue)
    {
        if (ReferenceEquals(renderer, null) || !renderer) yield break;

        Color color = renderer.material.color;
        float interval = Mathf.Infinity;

        while (interval > 0.01f)
        {
            color.a = Mathf.Lerp(color.a, targetValue, Time.deltaTime * 10.0f);
            renderer.material.color = color;

            interval = Mathf.Abs(color.a - targetValue);

            yield return null;
        }

        color.a = targetValue;
        renderer.material.shader = Shader.Find("Standard");
        renderer.material.color = color;
    }
    private void checkKeepObjectFromTransform(Transform trs)
    {
        if (_objects.Contains(trs) || ReferenceEquals(trs, TargetObject.transform)) return;

        if (trs.TryGetComponent(out Renderer renderer))
        {
            _objects.Add(trs.transform);
            StartCoroutine(setFadeOut(renderer, 0.1f));
        }
    }

    private IEnumerator GetMapInCoroutine(string objectKey, string key, IEnumerator coroutine)
    {
        if (!_coroutineMap.ContainsKey(objectKey))
        {
            Dictionary<string, IEnumerator> map = new Dictionary<string, IEnumerator>();
            map.Add(key, coroutine);
            _coroutineMap.Add(objectKey, map);
        }
        else
        {
            if (!_coroutineMap[objectKey].ContainsKey(key)) 
                _coroutineMap[objectKey].Add(key, coroutine);
        }

        return _coroutineMap[objectKey][key];
    }
}