namespace CShrimpSharp;

public readonly record struct Unit
{
    public static Unit Value => default;
    public override string ToString() => "()";
}
