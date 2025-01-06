namespace Vorcyc.Mathematics.Experimental.KalmanFilters;

using System.Numerics;

/// <summary>
/// ��ʾһ��һά�޼��������˲�����
/// </summary>
/// <typeparam name="T">��ֵ���ͣ�����ʵ�� INumber �ӿڡ�</typeparam>
/// <remarks>
/// �޼��������˲�����һ�ֵݹ��㷨������ͨ����Ϸ�����ϵͳ����ѧģ�ͺͲ������������ƶ�̬ϵͳ��״̬��������������ж��й㷺Ӧ�ã���������������ϵͳ���źŴ���;���ѧ�ȡ�
/// 
/// �޼��������˲����Ļ���˼���������޼��任��Unscented Transform�������������ϵͳ��������Ҫ���Ի���������������Ҫ���裺
/// 
/// 1. Ԥ�ⲽ�裺����ϵͳ�ķ�������ѧģ�ͣ�Ԥ�⵱ǰʱ�̵�ϵͳ״̬�����Э���
/// 2. ���²��裺���õ�ǰʱ�̵Ĳ������ݣ�����ϵͳ״̬�Ĺ��ƺ����Э���
/// 
/// �޼��������˲������ŵ��������ܹ��������������ṩ��ϵͳ״̬����ѹ��ƣ����Ҽ���Ч�ʸߣ��ʺ�ʵʱӦ�á�
/// 
/// ʾ�����룺
/// <code>
/// var A = 1.0f;
/// var B = 0.5f;
/// var H = 1.0f;
/// var Q = 0.1f;
/// var R = 0.1f;
/// var initialState = 0.0f;
/// var initialP = 1.0f;
/// 
/// var ukf = new UnscentedKalmanFilter1D&lt;float&gt;(A, B, H, Q, R, initialState, initialP);
/// 
/// float NonlinearStateTransitionFunction(float x, float u)
/// {
///     // ���������״̬ת�ƺ���
///     return x + u;
/// }
/// 
/// float NonlinearMeasurementFunction(float x)
/// {
///     // ��������Թ۲⺯��
///     return x;
/// }
/// 
/// float[] audioSamples = { 0.5f, 0.6f, 0.55f, 0.7f, 0.65f };
/// float[] filteredSamples = new float[audioSamples.Length];
/// 
/// for (int i = 0; i &lt; audioSamples.Length; i++)
/// {
///     var u = 0.0f; // ��������Ϊ0
///     var z = audioSamples[i]; // ��ǰ����ֵ
///     
///     var predictedState = ukf.Predict(u, NonlinearStateTransitionFunction);
///     var updatedState = ukf.Update(z, NonlinearMeasurementFunction);
///     
///     filteredSamples[i] = updatedState;
/// }
/// </code>
/// </remarks>
//[Filter(design: FilterDesignMethod.UnscentedKalman, structure: FilterStructure.Kalman, description: "һά�޼��������˲���")]
public class UnscentedKalmanFilter1D<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private T A; // ״̬ת��ϵ��
    private T B; // ��������ϵ��
    private T H; // �۲�ϵ��
    private T Q; // ��������Э����
    private T R; // ��������Э����
    private T P; // �������Э����
    private T x; // ״̬����

    /// <summary>
    /// ��ʼ���޼��������˲�����ʵ����
    /// </summary>
    /// <param name="A">״̬ת��ϵ����</param>
    /// <param name="B">��������ϵ����</param>
    /// <param name="H">�۲�ϵ����</param>
    /// <param name="Q">��������Э���</param>
    /// <param name="R">��������Э���</param>
    /// <param name="initialState">��ʼ״̬���ơ�</param>
    /// <param name="initialP">��ʼ�������Э���</param>
    public UnscentedKalmanFilter1D(T A, T B, T H, T Q, T R, T initialState, T initialP)
    {
        this.A = A;
        this.B = B;
        this.H = H;
        this.Q = Q;
        this.R = R;
        this.x = initialState;
        this.P = initialP;
    }

    /// <summary>
    /// ִ��Ԥ�ⲽ�衣
    /// </summary>
    /// <param name="u">�������롣</param>
    /// <param name="stateTransitionFunc">״̬ת�ƺ�����</param>
    /// <returns>Ԥ���״̬���ơ�</returns>
    public T Predict(T u, Func<T, T, T> stateTransitionFunc)
    {
        // ���� sigma ��
        var sigmaPoints = GenerateSigmaPoints(x, P);

        // ͨ��״̬ת�ƺ���Ԥ�� sigma ��
        var predictedSigmaPoints = TransformSigmaPoints(sigmaPoints, stateTransitionFunc, u);

        // ����Ԥ��״̬��Э����
        x = CalculateMean(predictedSigmaPoints);
        P = CalculateCovariance(predictedSigmaPoints, x) + Q;

        return x;
    }

    /// <summary>
    /// ִ�и��²��衣
    /// </summary>
    /// <param name="z">����ֵ��</param>
    /// <param name="measurementFunc">�۲⺯����</param>
    /// <returns>���º��״̬���ơ�</returns>
    public T Update(T z, Func<T, T> measurementFunc)
    {
        // ���� sigma ��
        var sigmaPoints = GenerateSigmaPoints(x, P);

        // ͨ���۲⺯��Ԥ�� sigma ��
        var predictedMeasurementSigmaPoints = TransformSigmaPoints(sigmaPoints, measurementFunc);

        // ����Ԥ�����ֵ��Э����
        var predictedMeasurement = CalculateMean(predictedMeasurementSigmaPoints);
        var measurementCovariance = CalculateCovariance(predictedMeasurementSigmaPoints, predictedMeasurement) + R;

        // ���㽻��Э����
        var crossCovariance = CalculateCrossCovariance(sigmaPoints, x, predictedMeasurementSigmaPoints, predictedMeasurement);

        // ���㿨��������
        var K = crossCovariance / measurementCovariance;

        // ����״̬��Э����
        x = x + K * (z - predictedMeasurement);
        P = P - K * measurementCovariance * K;

        return x;
    }

    private T[] GenerateSigmaPoints(T mean, T covariance)
    {
        // ���� sigma ���ʵ��
        int n = 1; // ״̬ά��
        T[] sigmaPoints = new T[2 * n + 1];
        T sqrtCovariance = T.Sqrt(covariance);

        sigmaPoints[0] = mean;
        sigmaPoints[1] = mean + sqrtCovariance;
        sigmaPoints[2] = mean - sqrtCovariance;

        return sigmaPoints;
    }

    private T[] TransformSigmaPoints(T[] sigmaPoints, Func<T, T, T> transformFunc, T u)
    {
        // ͨ��״̬ת�ƺ���Ԥ�� sigma ���ʵ��
        T[] transformedSigmaPoints = new T[sigmaPoints.Length];
        for (int i = 0; i < sigmaPoints.Length; i++)
        {
            transformedSigmaPoints[i] = transformFunc(sigmaPoints[i], u);
        }
        return transformedSigmaPoints;
    }

    private T[] TransformSigmaPoints(T[] sigmaPoints, Func<T, T> transformFunc)
    {
        // ͨ���۲⺯��Ԥ�� sigma ���ʵ��
        T[] transformedSigmaPoints = new T[sigmaPoints.Length];
        for (int i = 0; i < sigmaPoints.Length; i++)
        {
            transformedSigmaPoints[i] = transformFunc(sigmaPoints[i]);
        }
        return transformedSigmaPoints;
    }

    private T CalculateMean(T[] sigmaPoints)
    {
        // ���� sigma ��ľ�ֵ
        T mean = T.Zero;
        for (int i = 0; i < sigmaPoints.Length; i++)
        {
            mean += sigmaPoints[i];
        }
        return mean / T.CreateChecked(sigmaPoints.Length);
    }

    private T CalculateCovariance(T[] sigmaPoints, T mean)
    {
        // ���� sigma ���Э����
        T covariance = T.Zero;
        for (int i = 0; i < sigmaPoints.Length; i++)
        {
            T diff = sigmaPoints[i] - mean;
            covariance += diff * diff;
        }
        return covariance / T.CreateChecked(sigmaPoints.Length);
    }

    private T CalculateCrossCovariance(T[] sigmaPoints, T mean, T[] predictedMeasurementSigmaPoints, T predictedMeasurement)
    {
        // ���㽻��Э����
        T crossCovariance = T.Zero;
        for (int i = 0; i < sigmaPoints.Length; i++)
        {
            T diffState = sigmaPoints[i] - mean;
            T diffMeasurement = predictedMeasurementSigmaPoints[i] - predictedMeasurement;
            crossCovariance += diffState * diffMeasurement;
        }
        return crossCovariance / T.CreateChecked(sigmaPoints.Length);
    }
}
