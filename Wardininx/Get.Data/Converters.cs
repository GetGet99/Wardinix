namespace Get.Data;

public delegate TOut ForwardConverter<TIn, TOut>(TIn input);
public delegate TIn BackwardConverter<TIn, TOut>(TOut output);