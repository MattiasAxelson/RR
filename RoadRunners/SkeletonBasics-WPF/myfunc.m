function [x] = myfunc(a,b) 

figure(1)
set(0,'defaultfigureposition',[100 250 700 250])

x = plot (a,b);

xlabel 'Tid';
ylabel 'Vinkel';

hold;

end;
