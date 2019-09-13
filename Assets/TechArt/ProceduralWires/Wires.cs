//Code acquired from https://answers.unity.com/questions/375226/procedural-wirescablestubes.html

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wires : MonoBehaviour
{

    public GameObject next;
    public float gravity;
    public float radius;
    public int steps;

    public int subSplines;
    public float frequency;
    public float amplitude;

    public Material material;

    public GameObject energyEffectGO;
    public ParticleSystem energyEffectParticle;

    private Vector3 curPosition;
    private Vector3 nextPosition;
    private Vector3[] mainPointArr = new Vector3[0];

    private Vector3[,] subPointArr = new Vector3[0, 0];

    private GameObject[] children;

    private LineRenderer lineRenderer;
    private Vector3[] positions;
    private Vector3 startPosition;

    public Sequence sequence;
    public bool isEffectOn = false;

    void Start()
    {

    }

    public void CreateWire()
    {
        if (next)
        {
            nextPosition = next.transform.position;
            curPosition = transform.position;

            if (steps < 2) steps = 2;

            mainPointArr = new Vector3[steps + 1];

            for (int i = 0; i <= steps; i++)
            {
                mainPointArr[i] = DrawCosH(i);
            }

            if (subSplines < 1) subSplines = 1;

            subPointArr = new Vector3[subSplines, steps];

            for (int i = 0; i < subSplines; i++)
            {
                DrawSubSpline(i);
            }

            children = new GameObject[subSplines];

            for (int i = 0; i < subSplines; i++)
            {
                children[i] = new GameObject("Child0" + i);
                children[i].transform.parent = transform;
                LineRenderer temp = children[i].AddComponent<LineRenderer>();
                temp.positionCount = steps;
                temp.startWidth = radius;
                temp.endWidth = radius;
                temp.material = material;
                for (int j = 0; j < steps; j++)
                {
                    temp.SetPosition(j, subPointArr[i, j]);
                }
            }
        }
        
        StartEffect();
    }

    public void StartEffect()
    {
        isEffectOn = true;
        lineRenderer = GetComponentInChildren<LineRenderer>();
        
        if (!lineRenderer) return;
        
        positions = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(positions);
        Array.Reverse(positions);
        startPosition = next.transform.position;
        energyEffectParticle.Stop();
        energyEffectGO.transform.position = startPosition;
        energyEffectParticle.Play();
        DoEffect();
    }

    private void DoEffect()
    {
        if (sequence != null)
        {
            sequence.Kill();
            sequence = null;
        }
        
        sequence = DOTween.Sequence();
        sequence.Append(
            energyEffectGO.transform.DOPath(positions, 2f).OnComplete(delegate
              {
                  if (isEffectOn)
                  {
                      energyEffectParticle.Stop();
                      energyEffectGO.transform.position = startPosition;
                      energyEffectParticle.Play();
                      Invoke(nameof(DoEffect), Random.Range(5, 10));
                  }
              })
        );
    }

    public void CleanUp()
    {
        if (sequence != null)
        {
            sequence.Kill();
            sequence = null;
        }

        curPosition = Vector3.zero;
        nextPosition = Vector3.zero;
        mainPointArr = new Vector3[0];
        subPointArr = new Vector3[0, 0];
        children = new GameObject[subSplines];
    }

    public Vector3 DrawCosH(int i)
    {
        Vector3 direction = nextPosition - curPosition;
        int locStep = steps - 1;

        return curPosition + direction / locStep * i + new Vector3(0, gravity * (CosH((1.0f * i / locStep) - 0.5f) - CosH(0.5f)), 0);
    }

    float CosH(float t)
    {
        return (Mathf.Exp(t) + Mathf.Exp(-t)) / 2;
    }

    void DrawSubSpline(int i)
    {
        float locAmp = amplitude * Random.Range(0.5f, 1.2f) / 10;
        float locFreq = frequency * Random.Range(0.8f, 1.2f);
        float offset = Random.Range(0.0f, 1.0f);
        int cw = (Random.Range(0.0f, 1.0f) > 0.5 ? -1 : 1);

        for (int j = 0; j < steps; j++)
        {
            Vector3 direction = mainPointArr[j + 1] - mainPointArr[j];
            Quaternion rot = Quaternion.LookRotation(direction);

            subPointArr[i, j] = mainPointArr[j] + rot * (Vector3.up * Mathf.Sin(1.0f * j * locFreq / steps + offset) + Vector3.right * cw * Mathf.Cos(1.0f * j * locFreq / steps + offset)) * locAmp;
        }
    }
}
