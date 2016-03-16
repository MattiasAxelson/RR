function [x] = myfunc(a,b) 

x = plot (a,b);

hFig = figure(1);
set(hFig, 'Position', [50 280 1200 300])

title('DataAAAAAAA');
xlabel('Tid');
ylabel('Vinkel och puls');



%datetick(x,'HH');
end
