using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(AudioSource))]
public class SmoothUIMover : MonoBehaviour
{
    [Header("1. 위치 타겟 (Empty Objects)")]
    [SerializeField] private List<RectTransform> positionTransforms = new List<RectTransform>();

    [Header("2. 싱크할 UI 리스트 (Sync Objects 6개)")]
    [SerializeField] private RectTransform[] syncUIObjects = new RectTransform[6];

    [Header("3. 사운드 설정")]
    [SerializeField] private AudioClip moveSound; // 이동할 때 재생할 오디오 클립

    [Header("움직이는 오브젝트 크기 설정")]
    [SerializeField] private float movingObjectMinScale = 0.5f;

    [Header("싱크하는 오브젝트 크기 설정")]
    [SerializeField] private float syncObjectMaxScale = 1.2f;
    [SerializeField] private float syncObjectNormalScale = 1.0f;

    [Header("부드러움 설정 (Time)")]
    [SerializeField] private float posSmoothTime = 0.2f;
    [SerializeField] private float scaleSmoothTime = 0.1f;

    private RectTransform movingRectTransform;
    private AudioSource audioSource;
    private int currentIndex = 0;

    // SmoothDamp 속도 변수들
    private Vector2 posVelocity = Vector2.zero;
    private Vector3 movingScaleVelocity = Vector3.zero;
    private Vector3[] syncScaleVelocities = new Vector3[6];

    private Vector3 originalMovingScale;
    private Vector3 minMovingScale;

    void Start()
    {
        movingRectTransform = GetComponent<RectTransform>();

        // 스크립트가 붙은 오브젝트에서 AudioSource 컴포넌트를 가져옵니다.
        audioSource = GetComponent<AudioSource>();
        // UI 사운드용 설정 (3D 공간 음향 끄기)
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;

        if (movingRectTransform != null)
        {
            originalMovingScale = movingRectTransform.localScale;
            minMovingScale = originalMovingScale * movingObjectMinScale;

            if (positionTransforms.Count > 0 && positionTransforms[0] != null)
            {
                movingRectTransform.anchoredPosition = positionTransforms[0].anchoredPosition;
            }
        }

        for (int i = 0; i < syncScaleVelocities.Length; i++)
        {
            syncScaleVelocities[i] = Vector3.zero;
        }
    }

    void Update()
    {
        // --- 1. 키보드 입력 감지 및 인덱스 변경 ---
        bool hasIndexChanged = false;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                hasIndexChanged = true; // 인덱스가 변경됨을 기록
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            int maxIndex = Mathf.Min(positionTransforms.Count, syncUIObjects.Length) - 1;
            if (currentIndex < maxIndex)
            {
                currentIndex++;
                hasIndexChanged = true; // 인덱스가 변경됨을 기록
            }
        }

        // 💡 [사운드 재생] 인덱스가 실제로 바뀌었고, 사운드 파일이 등록되어 있다면 소리 재생
        if (hasIndexChanged && moveSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(moveSound);
        }

        // --- 2. 움직이는 오브젝트 위치 및 크기 처리 ---
        UpdateMovingObject();

        // --- 3. 싱크 오브젝트들 크기 처리 ---
        UpdateSyncObjectsScale();
    }

    private void UpdateMovingObject()
    {
        if (movingRectTransform == null || positionTransforms.Count == 0 || currentIndex >= positionTransforms.Count) return;
        if (positionTransforms[currentIndex] == null) return;

        Vector2 targetPos = positionTransforms[currentIndex].anchoredPosition;

        movingRectTransform.anchoredPosition = Vector2.SmoothDamp(
            movingRectTransform.anchoredPosition,
            targetPos,
            ref posVelocity,
            posSmoothTime
        );

        Vector3 targetScale;
        float distanceToTarget = Vector2.Distance(movingRectTransform.anchoredPosition, targetPos);

        if (distanceToTarget > 15.0f) targetScale = minMovingScale;
        else targetScale = originalMovingScale;

        movingRectTransform.localScale = Vector3.SmoothDamp(
            movingRectTransform.localScale,
            targetScale,
            ref movingScaleVelocity,
            scaleSmoothTime
        );
    }

    private void UpdateSyncObjectsScale()
    {
        for (int i = 0; i < syncUIObjects.Length; i++)
        {
            if (syncUIObjects[i] == null) continue;

            float targetScaleFloat = (i == currentIndex) ? syncObjectMaxScale : syncObjectNormalScale;
            Vector3 targetScale = Vector3.one * targetScaleFloat;

            syncUIObjects[i].localScale = Vector3.SmoothDamp(
                syncUIObjects[i].localScale,
                targetScale,
                ref syncScaleVelocities[i],
                scaleSmoothTime
            );
        }
    }
}