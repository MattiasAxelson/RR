function [x] = myfunc(a,b,c) 
% a = Tid 
% b = vinklar
% c = minsta vinkel
h = figure(1); set(gcf,'visible','off')

h.PaperUnits = 'inches';
h.PaperPosition = [0 0 14 4];

x = plot (a,b,a,c);
title('SuperGrafen');
xlabel('Tid');
ylabel('Vinkel');

saveas(h, 'Vinkelgraf.jpeg')
%heartRateCalc('20',10,'test.dat')

close(h);
end
