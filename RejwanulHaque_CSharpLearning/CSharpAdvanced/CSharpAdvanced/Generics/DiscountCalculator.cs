namespace CSharpAdvanced.Generics;

// Constraint to a Class
public class DiscountCalculator<TProduct> where TProduct : Product
{
    public float CalculateDiscount(TProduct product)
    {
        return product.Price * 0.1f;
    }
}