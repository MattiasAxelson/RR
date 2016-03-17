function [x] = myfunc(a,b) 

figure(1)
set(0,'defaultfigureposition',[0 0 800 400])

x = plot (a,b);

xlabel 'Tid';
ylabel 'Vinkel';



end
