function [x] = myfunc(a,b,c,d) 
% a = Tid 
% b = vinklar_FHK
% c = vinklar_SHK
h = figure(1); set(gcf,'visible','on')

h.PaperUnits = 'inches';
h.PaperPosition = [0 0 14 4];
length(a)
length(b)
length(c)
length(d)

x = plot (a,c,b,d);
title('SuperGrafen');
xlabel('Tid');
ylabel('Vinkel');
% legend('Vinklar FHK', 'Vinklar SHK', 'location', 'southwest');

saveas(h, 'Vinkelgraf.png')
close(h);
end


