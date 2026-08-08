public class PIDController
{
    private float _kp, _ki, _kd;
    private float _integral;
    private float _lastError;

    public PIDController(float kp, float ki, float kd)
    {
        _kp = kp;
        _ki = ki;
        _kd = kd;
    }

    public float Update(float error, float deltaTime)
    {
        _integral += error * deltaTime;
        float derivative = deltaTime > 0f ? (error - _lastError) / deltaTime : 0f;
        _lastError = error;
        return _kp * error + _ki * _integral + _kd * derivative;
    }

    public void Reset()
    {
        _integral = 0f;
        _lastError = 0f;
    }
}