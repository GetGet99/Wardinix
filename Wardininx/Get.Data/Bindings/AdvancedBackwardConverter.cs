namespace Get.Data.Bindings;
public delegate TIn AdvancedBackwardConverter<TIn, TOut>(TOut output, TIn oldInput);