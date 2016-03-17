function [x] = myfunc(a,b,c) 

h = figure(1)
set(figure(1),'Position',[100 60 1000 200])

x = plot (a,b,a,c);

xlabel 'TidJävel';
ylabel 'Vinkel';
saveas(h, 'Vinkelgraf.png')
end
