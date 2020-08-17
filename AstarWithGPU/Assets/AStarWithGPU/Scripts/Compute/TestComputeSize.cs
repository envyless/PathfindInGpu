using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestComputeSize : MonoBehaviour
{
    struct MT
    {
        int a;
        int b;
        int c;
        int d;/*
        int f;
        int g;
        int h;
        int i;*/

    };

    MT[] mt;

    // Start is called before the first frame update
    public ComputeShader cs;
    public int kernelID = 0;

    ComputeBuffer cb;

    const int size = 10000;

    void Start()
    {
        mt = new MT[size];
        for(int i = 0; i < size; ++i)
        {
            mt[i] = new MT();
        }

        Debug.LogError("byte size of array : " + mt.GetByteSize<MT>() * CommonUtils.Btye2Mb);

        kernelID = cs.FindKernel("MTestFunc");
        cb = new ComputeBuffer(size, CommonUtils.GetByteSize<MT>());
        cb.SetData(mt);
        cs.SetBuffer(kernelID, "cb", cb);
    }

    // Update is called once per frame
    void Update()
    {
        cs.Dispatch(kernelID, size, 1, 1);
        cb.GetData(mt);
    }

    private void OnDestroy()
    {
        cb.Release();
    }
}
