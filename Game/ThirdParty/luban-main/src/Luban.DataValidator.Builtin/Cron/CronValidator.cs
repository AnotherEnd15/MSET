using Luban.Datas;
using Luban.DataValidator.Builtin.Path;
using Luban.Defs;
using Luban.Types;
using Luban.Utils;
using Luban.Validator;
using Quartz;

namespace Luban.DataValidator.Builtin.Cron
{
[Validator("cron")]
public class CronValidator : DataValidatorBase
{
    private static readonly NLog.Logger s_logger = NLog.LogManager.GetCurrentClassLogger();
    
    private string _rawPattern;
    
    public CronValidator()
    {
    }

    public override void Compile(DefField field, TType type)
    {
        this._rawPattern = DefUtil.TrimBracePairs(Args);

        if (type is not TString)
        {
            ThrowCompileError(field, "只支持string类型");
        }
    }

    public override void Validate(DataValidatorContext ctx, TType type, DType data)
    {
        string value = ((DString)data).Value;
        if (value == "")
        {
            return;
        }

        if (!CronExpression.IsValidExpression(value))
        {
            s_logger.Error($"非法的cron表达式 {RecordPath} {value} {Source}");
            GenerationContext.Current.LogValidatorFail(this);
        }

    }

    private void ThrowCompileError(DefField def, string err)
    {
        throw new System.ArgumentException($"field:{def} {_rawPattern} 定义不合法. {err}");
    }
}
}