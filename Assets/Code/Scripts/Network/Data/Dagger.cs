using System;

[Serializable]
public class Dagger
{
    public int isDagger;
    public int attack;

    public int Sum => isDagger + attack;
}