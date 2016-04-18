function [x] = myfunc(a,b,c) 
% a = Tid 
% b = vinklar_FHK
% c = vinklar_SHK
h = figure(1); set(gcf,'visible','off')

h.PaperUnits = 'inches';
h.PaperPosition = [0 0 14 4];

x = plot (a,b,a,c);
title('SuperGrafen');
xlabel('Tid');
ylabel('Vinkel');
legend('Vinklar FHK', 'Vinklar SHK', 'location', 'southwest');

saveas(h, 'Vinkelgraf.jpeg')
close(h);
end
