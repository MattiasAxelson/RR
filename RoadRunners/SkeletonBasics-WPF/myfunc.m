function [x] = myfunc(a,b,c,d,e,f) 
% a = Tid 
% b = puls
% c = vinklar_FHK
% d = vinklar_SHK

%  a = 1:5;
%  b = [0,0,0,0,0];
%  c = [0,0,0,0,0];
%  d = [0,0,0,0,0];
%  e = [0,0,0,0,0];
%  f = [180,180,180,180,180];
%  
% YMatrix1 =[b;c;d;e;f];
length(a)

if length(a) < 2
    
 a = 1:5;
 b = [0,0,0,0,0];
 c = [0,0,0,0,0];
 d = [0,0,0,0,0];
 e = [0,0,0,0,0];
 f = [180,180,180,180,180];       
    % Create figure
 figure1 = figure('PaperUnits','inches');
set(gcf, 'visible', 'off');
% Create axes
axes1 = axes('Parent',figure1,'YGrid','on','XGrid','on',...
    'YColor',[0 0 0],...
    'XColor',[0 0 0],...
    'Position',[0.0660377358490566 0.16 0.905433735622415 0.799569600283172]);
box(axes1,'on');
hold(axes1,'on');

% Create ylabel
ylabel('Angle/Pulse','FontSize',14);

% Create xlabel
xlabel('Time','FontSize',14);

% Create multiple lines using matrix input to plot
plot1 = plot(a,b,a,c,a,d,a,e,a,f,'LineWidth',2,'Parent',axes1);
set(plot1(1),'DisplayName','Pulse','LineStyle','-.','Color',[1 0 0]);
set(plot1(2),'DisplayName','Knee angle','Color',[0 0 1]);
set(plot1(3),'DisplayName','Hip angle','Color',[1 0.843137264251709 0]);
set(plot1(4),'DisplayName','ChosenMinFHKAngle','Color',[0 0 0]);
set(plot1(5),'DisplayName','ChosenMaxFHKAngle','Color',[0 0 0]);

% Create legend
legend1 = legend(axes1,'show');
set(legend1,'Location','southwest','FontSize',9);

figure1.PaperUnits = 'inches';
figure1.PaperPosition = [0 0 14.5 4.5];

saveas(figure1, 'Vinkelgraf.jpeg');
close(figure1);
 
else
    
% Create figure
 figure1 = figure('PaperUnits','inches');
set(gcf, 'visible', 'off');
% Create axes
axes1 = axes('Parent',figure1,'YGrid','on','XGrid','on',...
    'YColor',[0 0 0],...
    'XColor',[0 0 0],...
    'Position',[0.0660377358490566 0.16 0.905433735622415 0.799569600283172]);
box(axes1,'on');
hold(axes1,'on');

% Create ylabel
ylabel('Angle/Pulse','FontSize',14);

% Create xlabel
xlabel('Time','FontSize',14);

% Create multiple lines using matrix input to plot
plot1 = plot(a,b,a,c,a,d,a,e,a,f,'LineWidth',2,'Parent',axes1);
set(plot1(1),'DisplayName','Pulse','LineStyle','-.','Color',[1 0 0]);
set(plot1(2),'DisplayName','Knee angle','Color',[0 0 1]);
set(plot1(3),'DisplayName','Hip angle','Color',[1 0.843137264251709 0]);
set(plot1(4),'DisplayName','ChosenMinFHKAngle','Color',[0 0 0]);
set(plot1(5),'DisplayName','ChosenMaxFHKAngle','Color',[0 0 0]);

% Create legend
legend1 = legend(axes1,'show');
set(legend1,'Location','southwest','FontSize',9);

figure1.PaperUnits = 'inches';
figure1.PaperPosition = [0 0 14.5 4.5];

saveas(figure1, 'Vinkelgraf.jpeg');
close(figure1);

%plotstyle = {'-r','-b','-ys'}
% aa=[a;a];
% bb=[b;b];
% zz=zeros(size(aa));
% 
% hs=surf(aa,bb,zz,bb, 'EdgeColor', 'interp')
% colormap(flipud(autumn))
% view(2)
% 
% HANDLE = gca;
% get(HANDLE);
% set(HANDLE, 'Color', [0,0.1,0.3]);
% hs.LineWidth=2
% title('SuperGrafen');

end


