function [x] = myfunc(a) 

%figure (1)
%x = plot (a);
%plot(a)
 %hold on


[Maxima,MaxIdx] = findpeaks(a);
aInv = 1.01*max(a) - a;
[Minima,MinIdx] = findpeaks(aInv);
Minima = a(MinIdx);
x = plot(Minima);
end
